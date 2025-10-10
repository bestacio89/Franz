output "gke_cluster_name" {
  value       = google_container_cluster.microservice_gke.name
  description = "Name of the created GKE cluster"
}

output "gke_cluster_endpoint" {
  value       = google_container_cluster.microservice_gke.endpoint
  description = "Endpoint of the GKE cluster"
}

output "microservice_service_name" {
  value       = kubernetes_service.microservice_service.metadata[0].name
  description = "Microservice service name"
}

output "microservice_service_external_ip" {
  value       = try(kubernetes_service.microservice_service.status[0].load_balancer[0].ingress[0].ip, null)
  description = "External LoadBalancer IP of the microservice"
}
