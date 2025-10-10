output "rabbitmq_service_ip" {
  description = "External IP for RabbitMQ service"
  value       = kubernetes_service.rabbitmq.status[0].load_balancer[0].ingress[0].ip
}

output "rabbitmq_amqp_port" {
  description = "AMQP port"
  value       = 5672
}

output "rabbitmq_management_port" {
  description = "Management UI port"
  value       = 15672
}

output "rabbitmq_user" {
  description = "RabbitMQ admin user"
  value       = var.rabbitmq_user
}

output "rabbitmq_password" {
  description = "RabbitMQ admin password"
  value       = var.rabbitmq_password
  sensitive   = true
}
