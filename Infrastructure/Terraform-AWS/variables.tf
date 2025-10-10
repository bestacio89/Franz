#######################################
# Global
#######################################
variable "aws_region" {
  description = "AWS region"
  type        = string
}

variable "microservice_name" {
  description = "Name of the microservice (used in resource naming)"
  type        = string
}

variable "common_tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}

#######################################
# Messaging
#######################################
variable "messaging_type" {
  description = "Messaging layer to deploy: Kafka, RabbitMQ, Both, or None"
  type        = string
  default     = "None"
}

variable "use_msk" {
  description = "Use Amazon MSK for Kafka (true) or self-managed alternative (false)"
  type        = bool
  default     = true
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

#######################################
# Database
#######################################
variable "database_type" {
  description = "Database type: postgres, mysql, mariadb, oracle, mssql, dynamodb"
  type        = string
}

variable "multi_db" {
  description = "Enable multiple databases support (true/false)"
  type        = bool
  default     = false
}

#######################################
# Compute
#######################################
variable "use_eks" {
  description = "If true, deploy EKS. If false, deploy ECS"
  type        = bool
  default     = true
}
