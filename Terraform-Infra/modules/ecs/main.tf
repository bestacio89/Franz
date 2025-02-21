resource "aws_ecs_cluster" "microservice_cluster" {
  count = var.use_eks ? 0 : 1
  name  = "ecs-microservice-cluster"
}

resource "aws_ecs_service" "microservice_service" {
  count           = var.use_eks ? 0 : 1
  name            = "microservice-service"
  cluster         = aws_ecs_cluster.microservice_cluster[0].id
  task_definition = aws_ecs_task_definition.microservice_task[0].arn
}
