#######################################
# Outputs
#######################################

output "rds_endpoint" {
  description = "RDS endpoint if using a relational database"
  value       = try(aws_db_instance.microservice_rds[0].endpoint, null)
  condition   = contains(["postgres", "mysql", "mariadb", "oracle", "sqlserver"], var.database_type)
}

output "rds_db_name" {
  description = "RDS database name"
  value       = try(aws_db_instance.microservice_rds[0].db_name, null)
  condition   = contains(["postgres", "mysql", "mariadb", "oracle", "sqlserver"], var.database_type)
}

output "dynamodb_table_name" {
  description = "DynamoDB table name if using NoSQL database"
  value       = try(aws_dynamodb_table.microservice_dynamodb[0].name, null)
  condition   = var.database_type == "dynamodb"
}

output "database_type_used" {
  description = "Type of database deployed"
  value       = var.database_type
}
