variable "gcp_region" {
  description = "GCP region for the Confluent Kafka cluster"
  type        = string
}

variable "kafka_cluster_name" {
  description = "Kafka cluster name"
  type        = string
}

variable "availability" {
  description = "Availability type for Kafka cluster (SINGLE_ZONE or MULTI_ZONE)"
  type        = string
  default     = "MULTI_ZONE"
}

variable "ckus" {
  description = "Capacity units for the dedicated Kafka cluster"
  type        = number
  default     = 1
}

variable "confluent_api_key" {
  description = "Confluent Cloud API Key"
  type        = string
  sensitive   = true
}

variable "confluent_api_secret" {
  description = "Confluent Cloud API Secret"
  type        = string
  sensitive   = true
}
