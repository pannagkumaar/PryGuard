import requests
from bs4 import BeautifulSoup

# Define the URL and payload
url = "https://us2.cp.mailhostbox.com/adduser.htm"
payload = {
    "xsrf_form_token": "b64ab66f6500ed7b5e0cdb6f2993412c",
    "firstName": "dumbo",
    "lastName": "kumar",
    "emailPrefix": "dumbol",
    "domainName": "pryguard.tech",
    "notifyEmailAddress": "suggme7@gmail.com",
    "selectedCountryCode": "US",
    "selectedLanguage": "en",
    "Submit": "Add User"
}

# Define the headers
headers = {
    "Cookie": "JSESSIONID=577B8877C220AC9F166B155E70C4705D-n2; DOMAIN_NAME=pryguard.tech; BrandingInformation=DotServe+Inc%7Ehttp%3A%2F%2Fdomains.get.tech; _hjSession_577384=eyJpZCI6IjdhZmI2ZDM2LWFhNTUtNGZhMC05OTViLWZiNjIzMDA4NDA1ZSIsImMiOjE3MjMwNDEwOTgzNDYsInMiOjAsInIiOjAsInNiIjowLCJzciI6MCwic2UiOjAsImZzIjoxLCJzcCI6MX0=; _hjSessionUser_577384=eyJpZCI6IjUwMDc3NDU4LTc1OTYtNThmMS04Y2U3LThhYTc3OTU1MjU0MCIsImNyZWF0ZWQiOjE3MjMwNDEwOTgzNDYsImV4aXN0aW5nIjp0cnVlfQ==",
    "Cache-Control": "max-age=0",
    "Sec-Ch-Ua": "\"Chromium\";v=\"125\", \"Not.A/Brand\";v=\"24\"",
    "Sec-Ch-Ua-Mobile": "?0",
    "Sec-Ch-Ua-Platform": "\"Linux\"",
    "Upgrade-Insecure-Requests": "1",
    "Origin": "https://us2.cp.mailhostbox.com",
    "Content-Type": "application/x-www-form-urlencoded",
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.6422.112 Safari/537.36",
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
    "Sec-Fetch-Site": "same-origin",
    "Sec-Fetch-Mode": "navigate",
    "Sec-Fetch-User": "?1",
    "Sec-Fetch-Dest": "document",
    "Referer": "https://us2.cp.mailhostbox.com/adduser.htm;jsessionid=577B8877C220AC9F166B155E70C4705D-n2",
    "Accept-Encoding": "gzip, deflate, br",
    "Accept-Language": "en-US,en;q=0.9",
    "Priority": "u=0, i"
}

# Send the POST request
response = requests.post(url, data=payload, headers=headers)

# Print the response text
soup = BeautifulSoup(response.text, 'html.parser')

print(soup)
# Find all paragraphs
paragraphs = soup.find_all('p')

# Debugging: print all paragraphs to inspect structure
for p in paragraphs:
    print(p.get_text(strip=True))

# Find the paragraph containing the password
password_tag = next((p for p in paragraphs if 'Password' in p.get_text()), None)

if password_tag:
    # Extract the password from the paragraph
    text = password_tag.get_text(strip=True)
    if ': ' in text:
        password = text.split(': ')[1].strip()
        print(f"The password for the account is: {password}")
    else:
        print("Password format is not as expected.")
else:
    print("Password not found in the response.")
