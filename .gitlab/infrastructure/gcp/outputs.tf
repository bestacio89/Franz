# ✅ Output the VPC ID
output "vpc_id" {
  value = module.networking.vpc_id
}

# ✅ Output the Database Endpoint (Cloud SQL, Firestore, MongoDB)
output "database_endpoint" {
  value = var.database_type == "cloudsql" ? module.database.cloudsql_endpoint :
          var.database_type == "firestore" ? "Firestore does not have a single endpoint" :
          var.database_type == "mongo_atlas" ? module.database.mongo_atlas_endpoint :
          var.database_type == "mongo_vm" ? module.database.mongo_vm_ip :
          "Database type not supported"
}

# ✅ Output the Kafka Broker (Confluent Cloud OR Self-Managed Kafka on GKE)
output "kafka_broker" {
  value = var.use_gke_kafka ? module.kafka.kafka_gke_service_ip : module.kafka.confluent_kafka_bootstrap
}

# ✅ Output the Microservice Service URL (GKE OR Cloud Run)
output "microservice_url" {
  value = var.use_gke ? module.microservices.gke_service_url : module.microservices.cloud_run_url
}
