resource "aws_mq_broker" "rabbitmq" {
  count = var.enable_rabbitmq ? 1 : 0

  broker_name        = "${var.microservice_name}-rabbitmq"
  engine_type        = "RabbitMQ"
  engine_version     = var.rabbitmq_version
  host_instance_type = var.rabbitmq_instance_type

  publicly_accessible = false
  auto_minor_version_upgrade = true
  deployment_mode    = "SINGLE_INSTANCE"

  user {
    username = var.rabbitmq_user
    password = var.rabbitmq_password
  }

  logs {
    general = true
  }

  security_groups = var.security_group_ids
  subnet_ids      = var.subnet_ids

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-rabbitmq" }
  )
}
