# ✅ Create VPC (Equivalent to AWS VPC)
resource "google_compute_network" "main_vpc" {
  name                    = "main-vpc"
  auto_create_subnetworks = false
}

# ✅ Create Public Subnet (Equivalent to AWS Subnet)
resource "google_compute_subnetwork" "public_subnet" {
  name          = "public-subnet"
  network       = google_compute_network.main_vpc.id
  ip_cidr_range = "10.0.1.0/24"
  region        = var.gcp_region
}

# ✅ Create Firewall Rules (Equivalent to AWS Security Groups)
resource "google_compute_firewall" "allow_http_https" {
  name    = "allow-http-https"
  network = google_compute_network.main_vpc.id

  allow {
    protocol = "tcp"
    ports    = ["80", "443"]
  }

  source_ranges = ["0.0.0.0/0"]
}

resource "google_compute_firewall" "allow_ssh" {
  name    = "allow-ssh"
  network = google_compute_network.main_vpc.id

  allow {
    protocol = "tcp"
    ports    = ["22"]
  }

  source_ranges = ["0.0.0.0/0"]
}

resource "google_compute_firewall" "allow_microservice" {
  name    = "allow-microservice"
  network = google_compute_network.main_vpc
