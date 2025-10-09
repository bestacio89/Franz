resource "google_container_cluster" "microservice_gke" {
  name     = "gke-microservice-cluster"
  location = var.gcp_region

  node_pool {
    name       = "default-pool"
    node_count = 2

    node_config {
      machine_type = "e2-medium"
      disk_size_gb = 20
      oauth_scopes = [
        "https://www.googleapis.com/auth/cloud-platform"
      ]
    }
  }
}

resource "kubernetes_deployment" "microservice_deployment" {
  metadata {
    name      = var.microservice_name
    namespace = "default"
  }

  spec {
    replicas = var.replicas

    selector {
      match_labels = {
        app = var.microservice_name
      }
    }

    template {
      metadata {
        labels = {
          app = var.microservice_name
        }
      }

      spec {
        container {
          name  = var.microservice_name
          image = var.container_image

          ports {
            container_port = var.container_port
          }

          # Environment variables
          dynamic "env" {
            for_each = var.env_vars
            content {
              name  = env.key
              value = env.value
            }
          }

          # Secrets (envFrom style)
          dynamic "env" {
            for_each = var.secrets
            content {
              name = env.key
              value_from {
                secret_key_ref {
                  name = env.value   # secret name
                  key  = env.key     # secret key inside secret
                }
              }
            }
          }

          resources {
            limits = {
              cpu    = "500m"
              memory = "256Mi"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "microservice_service" {
  metadata {
    name      = var.microservice_name
    namespace = "default"
  }

  spec {
    selector = {
      app = var.microservice_name
    }

    port {
      protocol    = "TCP"
      port        = var.service_port
      target_port = var.container_port
    }

    type = "LoadBalancer"
  }
}
