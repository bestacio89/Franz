variable "use_msk" {
  description = "Whether to deploy MSK (Kafka) cluster"
  type        = bool
  default     = true
}

variable "microservice_name" {
  description = "Name of the microservice (used for naming cluster)"
  type        = string
}

variable "kafka_version" {
  description = "Kafka version to deploy"
  type        = string
  default     = "3.7.0"
}

variable "kafka_instance_type" {
  description = "Instance type for Kafka brokers"
  type        = string
  default     = "kafka.t3.small"
}

variable "kafka_broker_nodes" {
  description = "Number of broker nodes in Kafka cluster"
  type        = number
  default     = 3
}

variable "subnet_ids" {
  description = "Subnets for Kafka brokers"
  type        = list(string)
}

variable "security_group_ids" {
  description = "Security groups for Kafka brokers"
  type        = list(string)
}

variable "common_tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
