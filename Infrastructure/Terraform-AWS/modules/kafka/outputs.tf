output "kafka_cluster_name" {
  description = "Name of the Kafka cluster"
  value       = try(aws_msk_cluster.kafka[0].cluster_name, null)
}

output "kafka_broker" {
  description = "Kafka broker bootstrap servers"
  value       = try(aws_msk_cluster.kafka[0].bootstrap_brokers_tls, null)
}

output "kafka_version" {
  description = "Kafka version deployed"
  value       = var.kafka_version
}
