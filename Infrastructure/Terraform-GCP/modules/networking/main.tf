# ✅ Create VPC
resource "google_compute_network" "main_vpc" {
  name                    = var.vpc_name
  auto_create_subnetworks = false
}

# ✅ Create Public Subnet
resource "google_compute_subnetwork" "public_subnet" {
  name          = "${var.vpc_name}-public-subnet"
  network       = google_compute_network.main_vpc.id
  ip_cidr_range = var.public_subnet_cidr
  region        = var.gcp_region
}

# ✅ Create Private Subnet
resource "google_compute_subnetwork" "private_subnet" {
  name          = "${var.vpc_name}-private-subnet"
  network       = google_compute_network.main_vpc.id
  ip_cidr_range = var.private_subnet_cidr
  region        = var.gcp_region
}

# ✅ Firewall for HTTP/HTTPS
resource "google_compute_firewall" "allow_http_https" {
  name    = "${var.vpc_name}-allow-http-https"
  network = google_compute_network.main_vpc.id

  allow {
    protocol = "tcp"
    ports    = ["80", "443"]
  }

  source_ranges = ["0.0.0.0/0"]
}

# ✅ Firewall for SSH (optional)
resource "google_compute_firewall" "allow_ssh" {
  count   = var.enable_ssh ? 1 : 0
  name    = "${var.vpc_name}-allow-ssh"
  network = google_compute_network.main_vpc.id

  allow {
    protocol = "tcp"
    ports    = ["22"]
  }

  source_ranges = var.allowed_ssh_ranges
}

# ✅ Firewall for Microservices
resource "google_compute_firewall" "allow_microservice" {
  name    = "${var.vpc_name}-allow-microservice"
  network = google_compute_network.main_vpc.id

  allow {
    protocol = "tcp"
    ports    = var.microservice_ports
  }

  source_ranges = ["0.0.0.0/0"]
}
