output "rabbitmq_id" {
  description = "RabbitMQ broker ID"
  value       = try(aws_mq_broker.rabbitmq[0].id, null)
}

output "rabbitmq_endpoints" {
  description = "RabbitMQ broker endpoints"
  value       = try(aws_mq_broker.rabbitmq[0].instances, null)
}

output "rabbitmq_version" {
  description = "RabbitMQ version deployed"
  value       = var.rabbitmq_version
}
