#!/usr/bin/env python3
"""
Generate BCrypt hash for a password with work factor 12
Used for seeding test users
"""

import sys

try:
    import bcrypt
except ImportError:
    print("❌ Error: bcrypt library not installed", file=sys.stderr)
    print("Install it with: pip3 install bcrypt", file=sys.stderr)
    sys.exit(1)

def generate_hash(password, work_factor=12):
    """Generate BCrypt hash with specified work factor"""
    salt = bcrypt.gensalt(rounds=work_factor)
    hash_bytes = bcrypt.hashpw(password.encode('utf-8'), salt)
    return hash_bytes.decode('utf-8')

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: generate-bcrypt-hash.py <password>", file=sys.stderr)
        sys.exit(1)
    
    password = sys.argv[1]
    work_factor = 12  # Match backend's work factor
    
    hash_value = generate_hash(password, work_factor)
    print(hash_value)
