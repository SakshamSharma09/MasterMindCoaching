// Debug script to check and fix user role
// Run this in browser console

// Check current auth data
const authData = localStorage.getItem('mastermind-auth');
if (authData) {
  const parsed = JSON.parse(authData);
  console.log('Current User:', parsed.user);
  console.log('Current Role:', parsed.user?.role);
  
  // If not admin, update to admin for testing
  if (parsed.user?.role !== 'Admin') {
    parsed.user.role = 'Admin';
    localStorage.setItem('mastermind-auth', JSON.stringify(parsed));
    console.log('✅ Updated role to Admin. Refresh the page and try accessing Finance again.');
  } else {
    console.log('✅ User already has Admin role. The issue might be something else.');
  }
} else {
  console.log('❌ No auth data found. Please log in first.');
}
