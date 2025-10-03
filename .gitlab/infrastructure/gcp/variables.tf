# Database Selection
variable "database_type" {
  description = "Choose RDS Engine: postgres, mysql, mariadb, oracle-se2, sqlserver-ex, dynamodb"
  type        = string
  default     = "postgres"
}

# Database Configurations
variable "db_version" {
  description = "Database engine version"
  type        = string
  default     = "13.4"  # PostgreSQL default
}

variable "db_instance_class" {
  description = "Instance type for RDS"
  type        = string
  default     = "db.t3.micro"
}

variable "db_allocated_storage" {
  description = "Storage size in GB"
  type        = number
  default     = 20
}

variable "db_name" {
  description = "Database name"
  type        = string
  default     = "microservice_db"
}

variable "db_username" {
  description = "Database username"
  type        = string
  default     = "admin"
}

variable "db_password" {
  description = "Database password"
  type        = string
  default     = "password123"
}

# DynamoDB Configuration
variable "dynamodb_table_name" {
  description = "DynamoDB table name"
  type        = string
  default     = "microservice-table"
}
