resource "aws_msk_cluster" "kafka" {
  count = var.use_msk ? 1 : 0

  cluster_name           = "${var.microservice_name}-kafka"
  kafka_version          = var.kafka_version
  number_of_broker_nodes = var.kafka_broker_nodes

  broker_node_group_info {
    instance_type   = var.kafka_instance_type
    client_subnets  = var.subnet_ids
    security_groups = var.security_group_ids
  }

  encryption_info {
    encryption_in_transit {
      client_broker = "TLS"
      in_cluster    = true
    }
  }

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-kafka" }
  )
}
