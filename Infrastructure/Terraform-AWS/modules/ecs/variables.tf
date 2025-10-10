#######################################
# Global
#######################################
variable "aws_region" {
  description = "AWS region to deploy resources"
  type        = string
}

variable "microservice_name" {
  description = "Name of the microservice (used in naming ECS resources)"
  type        = string
}

variable "container_image" {
  description = "Container image for the ECS microservice"
  type        = string
}

variable "container_port" {
  description = "Container port to expose"
  type        = number
  default     = 80
}

variable "use_eks" {
  description = "If true, deploy EKS. If false, deploy ECS Fargate."
  type        = bool
  default     = false
}

variable "common_tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}

#######################################
# Networking
#######################################
variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
  default     = "10.0.0.0/16"
}

#######################################
# ECS Variables
#######################################
variable "ecs_execution_role" {
  description = "IAM role ARN used by ECS task execution"
  type        = string
}

variable "ecs_service_sg" {
  description = "Security group ID for ECS service"
  type        = string
}

variable "ecs_cpu" {
  description = "CPU units for ECS task definition"
  type        = string
  default     = "256"
}

variable "ecs_memory" {
  description = "Memory (MB) for ECS task definition"
  type        = string
  default     = "512"
}

variable "ecs_desired_count" {
  description = "Number of desired ECS tasks"
  type        = number
  default     = 1
}
