#include <Servo.h>

Servo myservo;  // create servo object to control a servo

const int LEDs[3] = {17,20,19};

void setLeds(String message);
String getNextNumber(String text, int cursor);

void setup() {
  myservo.attach(22);
  Serial.begin(9600); // Serial monitor for debugging
  Serial1.setRX(1);
  Serial1.setTX(0);
  Serial1.begin(9600); // Bluetooth communication
  pinMode(LED_BUILTIN, OUTPUT); // Set LED pin as output
  pinMode(26,INPUT);
  for(int i=0;i<3;i++){
    pinMode(LEDs[i],OUTPUT);
  }
  myservo.write(0);
}

// message tokens
const char START_TOKEN = '?';
const char END_TOKEN = ';';
const char DELIMIT_TOKEN = '&';
const int CHAR_TIMEOUT = 20;

bool waitingForStartToken = true;
String messageBuffer = "";

long lastRun = millis();
bool outputValue = false;



void loop() {

  // handle Bluetooth link
  char nextData;
  if (Serial1.available()) {

    // check for start of message
    if (waitingForStartToken) {
      do {
        nextData = Serial1.read();
      } while((nextData != START_TOKEN) && Serial1.available());
      if (nextData == START_TOKEN) {
        Serial.println("message start");
        waitingForStartToken = false;
      }
    }

    // read command
    if (!waitingForStartToken && Serial1.available()){
      do {
        nextData = Serial1.read();
        Serial.println(nextData);
        messageBuffer += nextData;
      } while((nextData != END_TOKEN) && Serial1.available());

      // check if message complete
      if (nextData == END_TOKEN) {
        // remove last character
        messageBuffer = messageBuffer.substring(0, messageBuffer.length() - 1);
        Serial.println("message complete - " + messageBuffer);
        setLeds(messageBuffer);
        messageBuffer = "";
        waitingForStartToken = true;
      }

      // check for char timeout
      if (messageBuffer.length() > CHAR_TIMEOUT) {
        Serial.println("message data timeout - " + messageBuffer);
        messageBuffer = "";
        waitingForStartToken = true;
      }
    }
    
  }

  if ((millis() - lastRun) > 1000) {
    lastRun = millis();
    outputValue = !outputValue;
    //Serial.print(outputValue);
    char buf[20];
    if (outputValue) {
      sprintf(buf,"?L1|A%d",analogRead(26));
      digitalWrite(LED_BUILTIN, HIGH);   // turn the LED on
    }
    else {
      sprintf(buf,"?L0|A%d",analogRead(26));
      digitalWrite(LED_BUILTIN, LOW);   // turn the LED off
    }
    Serial.println(buf);
    Serial1.println(buf);

  }

  // Example: Send data back to Unity
  if (Serial.available()) {
    char receivedChar = Serial.read();
    Serial1.write(receivedChar);
  }

}

const int buzzer = 18;
bool buzzeron = false;
bool autom = true;
int limit = 200;

void loop1() {
  if (buzzeron){
    tone(buzzer, 1000); // Send 1KHz sound signal...
    delay(1000);        // ...for 1 sec
    noTone(buzzer);     // Stop sound...
    delay(1000);        // ...for 1sec
  }
  if (autom){
    String colors[3] = {"0" , "0" , "0"};
    if (analogRead(26) < limit){
      int a = map(analogRead(26),0,limit,0,255);
      colors[2] = 255 - a;
      colors[1] = a;
    }
    else {
      int a = map(analogRead(26),limit,2*limit,0,255);
      colors[1] = 255 - a;
      colors[0] = a;
    }
    dothecolors(colors);
  }
}
//________________________________________________________________________________________________________________
void setLeds(String message){
  int textCursor = 0;
  bool colsOk = true;
  String colors[3];

  /*
   * message should be in the format
   * r=xxx&y=xxx&g=xxx&a=x
   */
  
  if (message.startsWith("r=")){
    // correct starting message
    textCursor = 2;
    colors[0] = getNextNumber(message, textCursor);
    textCursor += colors[0].length() + 1;
    message=message.substring(textCursor);
  }
  else {
    colsOk = false;
  }
  if (message.startsWith("y=")){
    // correct starting message
    textCursor = 2;
    colors[1] = getNextNumber(message, textCursor);
    textCursor += colors[1].length() + 1;
    message=message.substring(textCursor);
  }
  else {
    colsOk = false;
  }
  if (message.startsWith("g=")){
    // correct starting message
    textCursor = 2;
    colors[2] = getNextNumber(message, textCursor);
    textCursor += colors[2].length() + 1;
    message=message.substring(textCursor);
  }
  else {
    colsOk = false;
  }
  if (message.startsWith("a=")){
    // correct starting message
    textCursor = 2;
    if(getNextNumber(message, textCursor) == "1")
    {
      autom = true;
      Serial.println("autom");
    }
    else
    {
      autom = false;
      Serial.println("not autom");
    }
  }
  else {
    colsOk = false;
  }
  if (colsOk) {
    dothecolors(colors);
  } 
}

void dothecolors(String colors[])
{
  //Serial.println("red = " + colors[0]);
  //Serial.println("yellow = " + colors[1]);
  //Serial.println("green = " + colors[2]);
  for(int i =0;i<3;i++){
    analogWrite(LEDs[i],(colors[i].toInt()));
   }
  if (colors[0] == "0"){
    myservo.write(0);
    buzzeron = false;
  }
  else{
    myservo.write(180);
    buzzeron = true;
  }
}

String getNextNumber(String text, int textCursor){
  String number = "";
  while((text[textCursor] >= '0') && (text[textCursor] <= '9') && (textCursor < text.length())){
    number += text[textCursor];
    textCursor ++;
  }
  return number;
}