resource "aws_mq_broker" "rabbitmq" {
  broker_name       = var.rabbitmq_name
  engine_type       = "RabbitMQ"
  engine_version    = var.engine_version
  host_instance_type = var.instance_type
  subnet_ids        = var.subnet_ids
  security_groups   = var.security_groups

  user {
    username = var.rabbitmq_user
    password = var.rabbitmq_password
  }

  logs {
    general = true
  }

  deployment_mode = "SINGLE_INSTANCE"
}
