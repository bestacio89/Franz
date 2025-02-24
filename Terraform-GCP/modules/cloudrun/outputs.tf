output "cloud_run_url" {
  value = google_cloud_run_service.microservice.status[0].url
}
