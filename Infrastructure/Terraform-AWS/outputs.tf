#######################################
# Networking
#######################################
output "vpc_id" {
  description = "VPC ID from networking module"
  value       = module.networking.vpc_id
}

#######################################
# Messaging
#######################################
output "kafka_broker" {
  description = "Kafka broker endpoints (if enabled)"
  value       = try(module.kafka[0].kafka_broker, null)
}

output "rabbitmq_endpoints" {
  description = "RabbitMQ endpoints (if enabled)"
  value       = try(module.rabbitmq[0].rabbitmq_endpoints, null)
}

#######################################
# Database
#######################################
output "database_endpoint" {
  description = "Database endpoint or table name"
  value       = try(module.database.db_endpoint, null)
}

#######################################
# Compute
#######################################
output "compute_cluster_name" {
  description = "Name of the compute cluster (EKS or ECS depending on toggle)"
  value       = try(module.ecs.cluster_name, null)
}

output "compute_cluster_endpoint" {
  description = "Cluster endpoint (EKS API or ECS cluster ARN)"
  value       = try(module.ecs.cluster_endpoint, null)
}
