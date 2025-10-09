output "rabbitmq_id" {
  value       = aws_mq_broker.rabbitmq.id
  description = "RabbitMQ broker ID"
}

output "rabbitmq_endpoints" {
  value       = aws_mq_broker.rabbitmq.instances
  description = "RabbitMQ endpoints (AMQP, Management, etc.)"
}

output "rabbitmq_user" {
  value       = var.rabbitmq_user
  description = "RabbitMQ username"
}

output "rabbitmq_password" {
  value       = var.rabbitmq_password
  description = "RabbitMQ password"
  sensitive   = true
}
