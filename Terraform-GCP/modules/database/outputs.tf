# 🔹 Cloud SQL
output "cloudsql_instance_name" {
  value       = try(google_sql_database_instance.microservice_db[0].name, null)
  description = "Name of the Cloud SQL instance"
}

output "cloudsql_connection_name" {
  value       = try(google_sql_database_instance.microservice_db[0].connection_name, null)
  description = "Connection string for Cloud SQL"
}

output "cloudsql_public_ip" {
  value       = try(google_sql_database_instance.microservice_db[0].public_ip_address, null)
  description = "Public IP address for Cloud SQL"
}

# 🔹 Firestore
output "firestore_database_name" {
  value       = try(google_firestore_database.microservice_firestore[0].name, null)
  description = "Name of the Firestore database"
}

# 🔹 Mongo Atlas
output "mongo_atlas_cluster_name" {
  value       = try(mongodbatlas_cluster.mongo_cluster[0].name, null)
  description = "Name of the MongoDB Atlas cluster"
}

output "mongo_atlas_cluster_uri" {
  value       = try(mongodbatlas_cluster.mongo_cluster[0].connection_strings[0].standard_srv, null)
  description = "Connection URI for MongoDB Atlas"
  sensitive   = true
}

# 🔹 Self-Managed Mongo VM
output "mongo_vm_name" {
  value       = try(google_compute_instance.mongo_vm[0].name, null)
  description = "Name of the self-managed Mongo VM"
}

output "mongo_vm_public_ip" {
  value       = try(google_compute_instance.mongo_vm[0].network_interface[0].access_config[0].nat_ip, null)
  description = "Public IP address of the self-managed Mongo VM"
}
