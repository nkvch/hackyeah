# Page snapshot

```yaml
- generic [ref=e5]:
  - generic [ref=e7]:
    - heading "UKNF Communication Platform" [level=1] [ref=e8]
    - paragraph [ref=e9]: Sign in to your account
  - generic [ref=e11]:
    - generic [ref=e12]:
      - generic [ref=e13]:
        - generic [ref=e14]:
          - text: Email
          - generic [ref=e15]: "*"
        - textbox "Email *" [ref=e16]
      - generic [ref=e17]:
        - generic [ref=e18]:
          - text: Password
          - generic [ref=e19]: "*"
        - generic [ref=e20]:
          - textbox "Enter your password" [ref=e21]
          - img [ref=e22]
      - generic [ref=e25]: Forgot password? (Coming soon)
      - button " Sign In" [disabled] [ref=e27]:
        - generic [ref=e28]: 
        - generic [ref=e29]: Sign In
    - paragraph [ref=e31]:
      - text: Don't have an account?
      - link "Register here" [ref=e32] [cursor=pointer]:
        - /url: /register
    - paragraph [ref=e34]:
      - text: Didn't receive activation email?
      - link "Resend activation" [ref=e35] [cursor=pointer]:
        - /url: /resend-activation
```