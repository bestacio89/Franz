########################################
# 🌍 Global Variables
########################################
variable "project_id" {
  description = "The GCP project ID"
  type        = string
}

variable "gcp_region" {
  description = "GCP deployment region"
  type        = string
  default     = "us-central1"
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

########################################
# 🗄️ Database
########################################
variable "database_type" {
  description = "Choose database: cloudsql, firestore, mongo_atlas, mongo_vm"
  type        = string
  validation {
    condition     = contains(["cloudsql", "firestore", "mongo_atlas", "mongo_vm"], var.database_type)
    error_message = "Allowed values: cloudsql, firestore, mongo_atlas, mongo_vm."
  }
}

########################################
# ☸️ Microservices
########################################
variable "use_gke" {
  description = "Deploy to GKE (true) or Cloud Run (false)"
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

########################################
# 📡 Messaging (Kafka + RabbitMQ)
########################################
variable "messaging_type" {
  description = "Which messaging stack to deploy: kafka | rabbitmq | both | none"
  type        = string
  default     = "kafka"
  validation {
    condition     = contains(["kafka", "rabbitmq", "both", "none"], var.messaging_type)
    error_message = "Allowed values: kafka, rabbitmq, both, none."
  }
}

# Kafka
variable "use_gke_kafka" {
  description = "Deploy Kafka on GKE (true) or Confluent Cloud (false)"
  type        = bool
  default     = false
}

variable "kafka_cluster_name" {
  description = "Kafka cluster name"
  type        = string
  default     = "microservice-kafka"
}

# RabbitMQ
variable "rabbitmq_name" {
  description = "RabbitMQ logical name"
  type        = string
  default     = "rabbitmq"
}

variable "rabbitmq_user" {
  description = "RabbitMQ admin username"
  type        = string
  default     = "admin"
}

variable "rabbitmq_password" {
  description = "RabbitMQ admin password"
  type        = string
  sensitive   = true
}
