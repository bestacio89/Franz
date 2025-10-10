#######################################
# Global
#######################################
variable "aws_region" {
  description = "AWS region to deploy resources"
  type        = string
}

variable "microservice_name" {
  description = "Name of the microservice (used in resource naming)"
  type        = string
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
# EKS
#######################################
variable "eks_cluster_version" {
  description = "EKS cluster version"
  type        = string
  default     = "1.30"
}

variable "eks_node_instance" {
  description = "Instance type for EKS nodes"
  type        = string
  default     = "t3.medium"
}

variable "eks_node_disk" {
  description = "EBS disk size for EKS nodes (GB)"
  type        = number
  default     = 20
}

variable "eks_node_desired" {
  description = "Desired node count"
  type        = number
  default     = 2
}

variable "eks_node_min" {
  description = "Minimum node count"
  type        = number
  default     = 1
}

variable "eks_node_max" {
  description = "Maximum node count"
  type        = number
  default     = 3
}

#######################################
# Database
#######################################
variable "database_type" {
  description = "Database type: postgres, mysql, mariadb, oracle, mssql, dynamodb"
  type        = string
}

variable "db_version" {
  description = "Database engine version"
  type        = string
  default     = "14"
}

variable "db_instance_class" {
  description = "Instance class for RDS database"
  type        = string
  default     = "db.t3.micro"
}
