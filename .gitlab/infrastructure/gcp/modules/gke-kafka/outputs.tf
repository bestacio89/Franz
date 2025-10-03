output "kafka_gke_service_ip" {
  value = kubernetes_service.kafka_service.status[0].load_balancer[0].ingress[0].ip
}
