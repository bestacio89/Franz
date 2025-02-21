output "vpc_id" {
  value = module.networking.vpc_id
}

output "database_endpoint" {
  value = module.database.db_endpoint
}

output "kafka_broker" {
  value = module.kafka.kafka_broker
}
