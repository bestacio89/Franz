resource "aws_vpc" "main_vpc" {
  cidr_block           = var.vpc_cidr
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-vpc" }
  )
}

resource "aws_subnet" "public_subnet" {
  vpc_id                  = aws_vpc.main_vpc.id
  cidr_block              = var.public_subnet_cidr
  map_public_ip_on_launch = true
  availability_zone       = var.availability_zone

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-public-subnet" }
  )
}

resource "aws_security_group" "microservice_sg" {
  vpc_id = aws_vpc.main_vpc.id

  tags = merge(
    var.common_tags,
    { "Name" = "${var.microservice_name}-sg" }
  )
}
