module "networking" {
  source = "./modules/networking"
}

# ✅ Kafka (MSK)
module "kafka" {
  source = "./modules/kafka"
  count  = var.messaging_type == "Kafka" || var.messaging_type == "Both" ? 1 : 0

  use_msk = var.use_msk
}

# ✅ RabbitMQ (Amazon MQ)
module "rabbitmq" {
  source = "./modules/rabbitmq"
  count  = var.messaging_type == "RabbitMQ" || var.messaging_type == "Both" ? 1 : 0

  broker_name     = "${var.microservice_name}-rabbitmq"
  subnet_ids      = [module.networking.public_subnet_id]
  security_groups = [module.networking.security_group_id]
  username        = var.rabbitmq_user
  password        = var.rabbitmq_password
}

# ✅ Database
module "database" {
  source        = "./modules/database"
  database_type = var.database_type
  multi_db      = var.multi_db
}

# ✅ Compute (EKS or ECS based on toggle)
module "compute" {
  source  = "./modules/ecs" # <— still called ecs module for now
  use_eks = var.use_eks
}
