variable "rabbitmq_name" {
  description = "RabbitMQ broker name"
  type        = string
  default     = "rabbitmq-broker"
}

variable "rabbitmq_user" {
  description = "RabbitMQ admin username"
  type        = string
}

variable "rabbitmq_password" {
  description = "RabbitMQ admin password"
  type        = string
  sensitive   = true
}

variable "instance_type" {
  description = "Instance type for RabbitMQ broker"
  type        = string
  default     = "mq.t3.micro"
}

variable "engine_version" {
  description = "RabbitMQ engine version"
  type        = string
  default     = "3.11.20"
}

variable "subnet_ids" {
  description = "Subnets to deploy RabbitMQ"
  type        = list(string)
}

variable "security_groups" {
  description = "Security groups for RabbitMQ broker"
  type        = list(string)
}
