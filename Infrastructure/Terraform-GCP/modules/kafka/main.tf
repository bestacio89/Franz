provider "confluent" {
  cloud_api_key    = var.confluent_api_key
  cloud_api_secret = var.confluent_api_secret
}

resource "confluent_kafka_cluster" "kafka" {
  name          = var.kafka_cluster_name
  cloud         = "GCP"
  region        = var.gcp_region
  availability  = var.availability

  dedicated {
    ckus = var.ckus
  }
}

resource "confluent_service_account" "kafka_sa" {
  display_name = "${var.kafka_cluster_name}-sa"
  description  = "Service account for Kafka cluster authentication"
}

resource "confluent_api_key" "kafka_api_key" {
  display_name       = "${var.kafka_cluster_name}-api-key"
  resource_id        = confluent_kafka_cluster.kafka.id
  resource_type      = "CLUSTER"
  service_account_id = confluent_service_account.kafka_sa.id
}
