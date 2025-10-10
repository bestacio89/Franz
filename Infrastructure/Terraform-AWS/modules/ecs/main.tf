provider "aws" {
  region = var.aws_region
}

# ==========================
# Networking (shared input)
# ==========================
# VPC and subnets are passed in from root module

# ==========================
# ECS Cluster
# ==========================
resource "aws_ecs_cluster" "microservice_cluster" {
  name = "${var.microservice_name}-ecs"

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-ecs" }
  )
}

# ==========================
# ECS Task Definition
# ==========================
resource "aws_ecs_task_definition" "microservice_task" {
  family                   = "${var.microservice_name}-task"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.ecs_cpu
  memory                   = var.ecs_memory
  execution_role_arn       = var.ecs_execution_role

  container_definitions = jsonencode([
    {
      name      = "app"
      image     = var.container_image
      essential = true
      portMappings = [
        {
          containerPort = var.container_port
          hostPort      = var.container_port
        }
      ]
    }
  ])
}

# ==========================
# ECS Service
# ==========================
resource "aws_ecs_service" "microservice_service" {
  name            = "${var.microservice_name}-service"
  cluster         = aws_ecs_cluster.microservice_cluster.id
  task_definition = aws_ecs_task_definition.microservice_task.arn
  desired_count   = var.ecs_desired_count

  network_configuration {
    subnets          = var.subnet_ids
    assign_public_ip = true
    security_groups  = [var.ecs_service_sg]
  }

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-ecs-service" }
  )
}
