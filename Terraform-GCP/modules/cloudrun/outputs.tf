output "service_name" {
  value       = google_cloud_run_service.microservice.name
  description = "The name of the Cloud Run service"
}

output "service_region" {
  value       = google_cloud_run_service.microservice.location
  description = "Region where the service is deployed"
}

output "service_status" {
  value       = google_cloud_run_service.microservice.status[0].conditions
  description = "Deployment status conditions of the Cloud Run service"
}

output "service_url" {
  value       = google_cloud_run_service.microservice.status[0].url
  description = "Public URL of the Cloud Run service"
}
