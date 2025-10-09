variable "enable_rabbitmq" {
  description = "Whether to deploy RabbitMQ broker"
  type        = bool
  default     = false
}

variable "microservice_name" {
  description = "Microservice name (used for naming resources)"
  type        = string
}

variable "common_tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

variable "rabbitmq_version" {
  description = "RabbitMQ engine version"
  type        = string
  default     = "3.10.20"
}

variable "rabbitmq_instance_type" {
  description = "Instance type for RabbitMQ broker"
  type        = string
  default     = "mq.t3.micro"
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

variable "security_group_ids" {
  description = "Security groups for RabbitMQ broker"
  type        = list(string)
}

variable "subnet_ids" {
  description = "Subnets for RabbitMQ broker"
  type        = list(string)
}
