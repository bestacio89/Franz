resource "aws_msk_cluster" "kafka" {
  count = var.use_msk ? 1 : 0

  cluster_name           = "microservice-kafka"
  kafka_version          = "3.2.0"
  number_of_broker_nodes = 3
  broker_node_group_info {
    instance_type = "kafka.t3.small"
    client_subnets = [aws_subnet.public_subnet.id]
    security_groups = [aws_security_group.microservice_sg.id]
  }
}
