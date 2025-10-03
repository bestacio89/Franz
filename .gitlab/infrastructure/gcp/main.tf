# ✅ Global Variables
variable "gcp_region" {
  description = "GCP deployment region"
  type        = string
}

variable "database_type" {
  description = "Choose database: cloudsql, firestore, mongo_atlas, mongo_vm"
  type        = string
}

variable "use_gke" {
  description = "Deploy to GKE (true) or Cloud Run (false)"
  type        = bool
  default     = false
}

variable "use_gke_kafka" {
  description = "Deploy Kafka on GKE (true) or Confluent Cloud (false)"
  type        = bool
  default     = false
}

variable "microservice_name" {
  description = "Name of the microservice"
  type        = string
}

variable "container_image" {
  description = "Container image for microservice"
  type        = string
}

variable "kafka_cluster_name" {
  description = "Kafka cluster name"
  type        = string
  default     = "microservice-kafka"
}

# ====================================
# ✅ 1. Networking (VPC, Firewall, Subnets)
# ====================================
module "networking" {
  source     = "./modules/networking"
  gcp_region = var.gcp_region
}

# ====================================
# ✅ 2. Database (Cloud SQL, Firestore, MongoDB)
# ====================================
module "database" {
  source        = "./modules/database"
  database_type = var.database_type
  gcp_region    = var.gcp_region
}

# ====================================
# ✅ 3. Kafka (Confluent Cloud OR Self-Managed Kafka on GKE)
# ====================================
module "kafka" {
  source             = "./modules/kafka"
  use_gke_kafka      = var.use_gke_kafka
  kafka_cluster_name = var.kafka_cluster_name
  gcp_region         = var.gcp_region
}

# ====================================
# ✅ 4. Microservices (GKE OR Cloud Run)
# ====================================
module "microservices" {
  source             = "./modules/microservices"
  use_gke           = var.use_gke
  gcp_region        = var.gcp_region
  microservice_name = var.microservice_name
  container_image   = var.container_image
}
