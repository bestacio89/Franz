output "gke_service_url" {
  value = kubernetes_service.microservice_service.status[0].load_balancer[0].ingress[0].ip
}
