#######################################
# EKS
#######################################
output "eks_cluster_name" {
  description = "EKS cluster name"
  value       = module.eks.cluster_name
}

output "eks_cluster_endpoint" {
  description = "EKS cluster API endpoint"
  value       = module.eks.cluster_endpoint
}

#######################################
# Database
#######################################
output "database_endpoint" {
  description = "Database endpoint (RDS endpoint or DynamoDB table name)"
  value       = try(module.database.db_endpoint, null)
}

output "database_type" {
  description = "Database type in use"
  value       = var.database_type
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

output "messaging_type" {
  description = "Type of messaging layer deployed"
  value       = var.messaging_type
}
