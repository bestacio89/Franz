variable "gcp_region" {}
variable "kafka_cluster_name" {}

provider "confluent" {
  cloud_api_key    = var.confluent_api_key
  cloud_api_secret = var.confluent_api_secret
}

resource "confluent_kafka_cluster" "kafka" {
  name           = var.kafka_cluster_name
  cloud         = "GCP"
  region        = var.gcp_region
  availability  = "MULTI_ZONE"
  dedicated {
    ckus = 1
  }
}

resource "confluent_service_account" "kafka_sa" {
  display_name = "Kafka Service Account"
  description  = "Service account for Kafka authentication"
}

resource "confluent_api_key" "kafka_api_key" {
  display_name    = "Kafka API Key"
  resource_id     = confluent_kafka_cluster.kafka.id
  resource_type   = "CLUSTER"
  service_account_id = confluent_service_account.kafka_sa.id
}
