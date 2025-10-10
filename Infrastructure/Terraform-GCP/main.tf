# Microservices deployment - choose Cloud Run or GKE
module "microservices" {
  source = var.use_gke ? "./modules/gke" : "./modules/cloudrun"

  project_id        = var.project_id
  gcp_region        = var.gcp_region
  environment       = var.environment
  microservice_name = "${var.microservice_name}-${var.environment}"
  container_image   = var.container_image
  vpc_id            = module.networking.vpc_id
}

# Messaging choice
module "kafka" {
  count  = var.messaging_type == "kafka" || var.messaging_type == "both" ? 1 : 0
  source = var.use_gke ? "./modules/gke-kafka" : "./modules/kafka"

  project_id         = var.project_id
  gcp_region         = var.gcp_region
  kafka_cluster_name = "${var.kafka_cluster_name}-${var.environment}"
}

module "rabbitmq" {
  count  = var.messaging_type == "rabbitmq" || var.messaging_type == "both" ? 1 : 0
  source = var.use_gke ? "./modules/gke-RabbitMQ" : "./modules/RabbitMQ"

  project_id  = var.project_id
  gcp_region  = var.gcp_region
  name        = "${var.rabbitmq_name}-${var.environment}"
  admin_user  = var.rabbitmq_user
  admin_pass  = var.rabbitmq_password
}
