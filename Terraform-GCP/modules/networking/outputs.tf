output "vpc_id" {
  value       = google_compute_network.main_vpc.id
  description = "ID of the created VPC"
}

output "public_subnet_id" {
  value       = google_compute_subnetwork.public_subnet.id
  description = "ID of the public subnet"
}

output "private_subnet_id" {
  value       = google_compute_subnetwork.private_subnet.id
  description = "ID of the private subnet"
}

output "firewall_rules" {
  value = {
    http_https    = google_compute_firewall.allow_http_https.name
    ssh           = var.enable_ssh ? google_compute_firewall.allow_ssh[0].name : null
    microservice  = google_compute_firewall.allow_microservice.name
  }
  description = "Firewall rules created in this VPC"
}
