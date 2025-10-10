#######################################
# Global Variables
#######################################

variable "microservice_name" {
  description = "Name of the microservice (used for tagging and naming resources)"
  type        = string
}

variable "database_type" {
  description = <<EOT
Database type to deploy.
Options:
- Relational: postgres, mysql, mariadb, oracle, sqlserver
- NoSQL: dynamodb
EOT
  type = string
}

#######################################
# RDS Variables
#######################################

variable "db_allocated_storage" {
  description = "Allocated storage in GB for RDS instances"
  type        = number
  default     = 20
}

variable "db_instance_class" {
  description = "Instance class for RDS (e.g., db.t3.micro, db.t3.medium)"
  type        = string
  default     = "db.t3.micro"
}

variable "db_version" {
  description = "Database engine version for RDS"
  type        = string
}

variable "db_name" {
  description = "Database name for RDS"
  type        = string
}

variable "db_username" {
  description = "Master username for RDS"
  type        = string
}

variable "db_password" {
  description = "Master password for RDS"
  type        = string
  sensitive   = true
}

variable "db_public_access" {
  description = "Should the RDS instance be publicly accessible?"
  type        = bool
  default     = false
}

#######################################
# DynamoDB Variables
#######################################

variable "dynamodb_table_name" {
  description = "Base name of the DynamoDB table"
  type        = string
  default     = "microservice-table"
}

#######################################
# Common Tags
#######################################

variable "common_tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
