resource "google_cloud_run_service" "microservice" {
  name     = var.microservice_name
  location = var.gcp_region

  template {
    spec {
      containers {
        image = var.container_image
        ports {
          container_port = var.container_port
        }
        resources {
          limits = {
            cpu    = var.cpu_limit
            memory = var.memory_limit
          }
        }
      }
    }
  }

  traffic {
    percent         = 100
    latest_revision = true
  }
}

resource "google_cloud_run_service_iam_policy" "allow_unauthenticated" {
  count    = var.allow_unauthenticated ? 1 : 0
  location = google_cloud_run_service.microservice.location
  service  = google_cloud_run_service.microservice.name

  policy_data = <<EOT
{
  "bindings": [
    {
      "role": "roles/run.invoker",
      "members": ["allUsers"]
    }
  ]
}
EOT
}
