output "kafka_cluster_id" {
  value       = confluent_kafka_cluster.kafka.id
  description = "Kafka cluster ID"
}

output "kafka_bootstrap_endpoint" {
  value       = confluent_kafka_cluster.kafka.bootstrap_endpoint
  description = "Kafka bootstrap endpoint"
}

output "kafka_api_key" {
  value       = confluent_api_key.kafka_api_key.id
  description = "Kafka API Key"
  sensitive   = true
}

output "kafka_api_secret" {
  value       = confluent_api_key.kafka_api_key.secret
  description = "Kafka API Secret"
  sensitive   = true
}
