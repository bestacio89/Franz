# ==========================
# Provider & Authentication
# ==========================
provider "aws" {
  region = var.aws_region
}

# Kubernetes provider for EKS workloads
provider "kubernetes" {
  host                   = module.eks.cluster_endpoint
  cluster_ca_certificate = base64decode(module.eks.cluster_certificate_authority_data)
  token                  = data.aws_eks_cluster_auth.cluster.token
}

data "aws_eks_cluster_auth" "cluster" {
  name = module.eks.cluster_name
}

# ==========================
# Networking
# ==========================
module "networking" {
  source   = "./modules/networking"
  vpc_cidr = var.vpc_cidr

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-vpc" }
  )
}

# ==========================
# EKS Cluster
# ==========================
module "eks" {
  source          = "terraform-aws-modules/eks/aws"
  cluster_name    = "${var.microservice_name}-eks"
  cluster_version = var.eks_cluster_version
  subnets         = module.networking.private_subnets
  vpc_id          = module.networking.vpc_id

  node_groups = {
    default = {
      desired_capacity = var.eks_node_desired
      max_capacity     = var.eks_node_max
      min_capacity     = var.eks_node_min
      instance_types   = [var.eks_node_instance]
      disk_size        = var.eks_node_disk
    }
  }

  enable_irsa = true

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-eks" }
  )
}

# ==========================
# Database Layer
# ==========================
module "database" {
  source               = "./modules/database"
  microservice_name    = var.microservice_name
  database_type        = var.database_type
  db_version           = var.db_version
  db_instance_class    = var.db_instance_class
  db_allocated_storage = var.db_allocated_storage
  db_name              = var.db_name
  db_username          = var.db_username
  db_password          = var.db_password
  vpc_id               = module.networking.vpc_id
  subnet_ids           = module.networking.private_subnets
  common_tags          = var.common_tags
}

# ==========================
# Messaging Layer
# ==========================
module "kafka" {
  source     = "./modules/kafka"
  count      = var.messaging_type == "Kafka" || var.messaging_type == "Both" ? 1 : 0
  vpc_id     = module.networking.vpc_id
  subnet_ids = module.networking.private_subnets
  tags       = var.common_tags
}

module "rabbitmq" {
  source            = "./modules/rabbitmq"
  count             = var.messaging_type == "RabbitMQ" || var.messaging_type == "Both" ? 1 : 0
  vpc_id            = module.networking.vpc_id
  subnet_ids        = module.networking.private_subnets
  rabbitmq_user     = var.rabbitmq_user
  rabbitmq_password = var.rabbitmq_password
  tags              = var.common_tags
}

# ==========================
# Outputs
# ==========================
output "eks_cluster_name" {
  description = "EKS cluster name"
  value       = module.eks.cluster_name
}

output "eks_cluster_endpoint" {
  description = "EKS cluster API endpoint"
  value       = module.eks.cluster_endpoint
}

output "database_endpoint" {
  description = "Database endpoint (RDS or DynamoDB table name)"
  value       = try(module.database.db_endpoint, null)
}

output "kafka_broker" {
  description = "Kafka broker endpoints (if Kafka enabled)"
  value       = try(module.kafka[0].kafka_broker, null)
}

output "rabbitmq_endpoints" {
  description = "RabbitMQ endpoints (if RabbitMQ enabled)"
  value       = try(module.rabbitmq[0].rabbitmq_endpoints, null)
}
