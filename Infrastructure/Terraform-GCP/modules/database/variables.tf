variable "database_type" {
  description = "Type of database (cloudsql, firestore, mongo_atlas, mongo_vm)"
  type        = string
}

# 🔹 Common
variable "db_region" {
  description = "GCP region where the DB will be deployed"
  type        = string
}

variable "db_username" {
  description = "Database admin username"
  type        = string
}

variable "db_password" {
  description = "Database admin password"
  type        = string
  sensitive   = true
}

# 🔹 Cloud SQL
variable "db_instance_name" {
  description = "Name of the Cloud SQL instance (if using Cloud SQL)"
  type        = string
  default     = "microservice-db"
}

variable "db_version" {
  description = "Database version for Cloud SQL (e.g. POSTGRES_14, MYSQL_8_0)"
  type        = string
  default     = "POSTGRES_14"
}

variable "db_tier" {
  description = "Machine type (e.g., db-f1-micro, db-n1-standard-1)"
  type        = string
  default     = "db-f1-micro"
}

# 🔹 Firestore
variable "firestore_enabled" {
  description = "Enable Firestore if using NoSQL"
  type        = bool
  default     = false
}

# 🔹 Mongo Atlas
variable "mongodb_atlas_public_key" {
  description = "MongoDB Atlas Public Key"
  type        = string
  default     = ""
}

variable "mongodb_atlas_private_key" {
  description = "MongoDB Atlas Private Key"
  type        = string
  default     = ""
}

variable "mongodb_org_id" {
  description = "MongoDB Atlas Organization ID"
  type        = string
  default     = ""
}
