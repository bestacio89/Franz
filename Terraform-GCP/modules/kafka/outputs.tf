output "confluent_kafka_bootstrap" {
  value = confluent_kafka_cluster.kafka.bootstrap_servers
}
