# RDS Database Instance (PostgreSQL, MySQL, MariaDB, Oracle, MSSQL)
resource "aws_rds_instance" "microservice_db" {
  count             = var.database_type != "dynamodb" ? 1 : 0
  allocated_storage = var.db_allocated_storage
  storage_type      = "gp2"
  engine           = var.database_type
  engine_version   = var.db_version
  instance_class   = var.db_instance_class
  db_name         = var.db_name
  username        = var.db_username
  password        = var.db_password
  publicly_accessible = true
  skip_final_snapshot = true
}

# DynamoDB Table (NoSQL Option)
resource "aws_dynamodb_table" "microservice_dynamodb" {
  count        = var.database_type == "dynamodb" ? 1 : 0
  name         = var.dynamodb_table_name
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "id"

  attribute {
    name = "id"
    type = "S"
  }
}
