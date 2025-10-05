#!/bin/bash

# Script to create internal UKNF users for testing
# This script directly inserts users into the database with pre-hashed passwords

set -e

echo "🔐 UKNF Internal Users Seeding Script"
echo "======================================"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if docker-compose is running
if ! docker-compose ps | grep -q "uknf-backend.*Up"; then
    echo -e "${RED}❌ Backend container is not running. Please start it with 'docker-compose up -d'${NC}"
    exit 1
fi

# Test password for all internal users (ONLY FOR TESTING!)
TEST_PASSWORD="UknfAdmin123!"

echo -e "${YELLOW}ℹ️  This script will create internal UKNF users with the password: ${TEST_PASSWORD}${NC}"
echo ""

# Array of internal users to create
declare -a USERS=(
    "Jan|Kowalski|jan.kowalski@uknf.gov.pl|+48501234567|90010112345|2345"
    "Anna|Nowak|anna.nowak@uknf.gov.pl|+48501234568|85050554321|4321"
    "Piotr|Wiśniewski|piotr.wisniewski@uknf.gov.pl|+48501234569|92030998765|8765"
    "Maria|Wójcik|maria.wojcik@uknf.gov.pl|+48501234570|88070776543|6543"
    "Tomasz|Kamiński|tomasz.kaminski@uknf.gov.pl|+48501234571|91110134567|4567"
)

echo "📋 Users to be created:"
for user_data in "${USERS[@]}"; do
    IFS='|' read -r first_name last_name email phone pesel pesel_last4 <<< "$user_data"
    echo "  • $first_name $last_name <$email>"
done
echo ""

# BCrypt hash for "UknfAdmin123!" with work factor 12
# Generated using: python3 -c "import bcrypt; print(bcrypt.hashpw(b'UknfAdmin123!', bcrypt.gensalt(rounds=12)).decode('utf-8'))"
PASSWORD_HASH='$2b$12$4w3ekvRKoIk1wNPaLx6lY.bBSt.Ml99hF1E8lml/KS6T0J6H6iEX6'

echo ""
echo "🚀 Creating internal users..."
echo ""

SUCCESS_COUNT=0
FAILED_COUNT=0

# Create each user
for user_data in "${USERS[@]}"; do
    IFS='|' read -r first_name last_name email phone pesel pesel_last4 <<< "$user_data"
    
    echo -n "Creating $first_name $last_name... "
    
    # Generate UUID for user
    USER_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
    
    # Create SQL to insert user (properly escaping the BCrypt hash)
    SQL="INSERT INTO \"Users\" (
        \"Id\", 
        \"FirstName\", 
        \"LastName\", 
        \"Email\", 
        \"Phone\", 
        \"PeselEncrypted\", 
        \"PeselLast4\", 
        \"PasswordHash\", 
        \"LastPasswordChangeDate\", 
        \"UserType\", 
        \"IsActive\", 
        \"MustChangePassword\", 
        \"CreatedDate\", 
        \"UpdatedDate\"
    ) VALUES (
        '$USER_ID',
        '$first_name',
        '$last_name',
        '$email',
        '$phone',
        'ENCRYPTED_${pesel}',
        '$pesel_last4',
        E'$PASSWORD_HASH',
        NOW(),
        'Internal',
        true,
        false,
        NOW(),
        NOW()
    ) ON CONFLICT (\"Email\") DO NOTHING;"
    
    # Execute SQL
    if docker-compose exec -T postgres psql -U postgres -d uknf_platform -c "$SQL" >/dev/null 2>&1; then
        echo -e "${GREEN}✓${NC}"
        ((SUCCESS_COUNT++))
    else
        echo -e "${RED}✗ (may already exist)${NC}"
        ((FAILED_COUNT++))
    fi
done

echo ""
echo "======================================"
echo -e "${GREEN}✅ Created $SUCCESS_COUNT user(s)${NC}"
if [ $FAILED_COUNT -gt 0 ]; then
    echo -e "${YELLOW}⚠️  $FAILED_COUNT user(s) failed (likely already exist)${NC}"
fi
echo ""

# Display credentials
echo "🔐 INTERNAL USER CREDENTIALS (FOR TESTING ONLY)"
echo "================================================"
echo ""
echo -e "${YELLOW}Common Password for all users: ${TEST_PASSWORD}${NC}"
echo ""
echo "Users:"
for user_data in "${USERS[@]}"; do
    IFS='|' read -r first_name last_name email phone pesel pesel_last4 <<< "$user_data"
    echo "  📧 Email:    $email"
    echo "     Password: $TEST_PASSWORD"
    echo "     Name:     $first_name $last_name"
    echo "     Role:     UKNF Employee (Internal)"
    echo ""
done

echo "================================================"
echo "ℹ️  You can now log in to the platform using any of these credentials"
echo "ℹ️  Internal users should see different UI/features than external users"
echo ""

# Save credentials to file
CREDS_FILE="test-data/internal-users-credentials.txt"
echo "💾 Saving credentials to: $CREDS_FILE"
cat > "$CREDS_FILE" << EOF
UKNF INTERNAL USERS - TEST CREDENTIALS
======================================
Generated: $(date)

Common Password: $TEST_PASSWORD

Users:
$(for user_data in "${USERS[@]}"; do
    IFS='|' read -r first_name last_name email phone pesel pesel_last4 <<< "$user_data"
    echo ""
    echo "Email:    $email"
    echo "Password: $TEST_PASSWORD"
    echo "Name:     $first_name $last_name"
    echo "Role:     UKNF Employee (Internal)"
done)

======================================
⚠️  FOR TESTING ONLY - DO NOT USE IN PRODUCTION
EOF

echo "✅ Credentials saved to $CREDS_FILE"
echo ""
