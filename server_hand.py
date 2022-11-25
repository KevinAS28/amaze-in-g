import socket, time
import cv2
import mediapipe as mp
from threading import Thread
from math import sqrt
import json

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_hands = mp.solutions.hands  

message = "test"
running = True

def get_fingers_up_numbers(hand_landmarks):

    target_landmarks = [
      mp_hands.HandLandmark.THUMB_TIP,
      mp_hands.HandLandmark.INDEX_FINGER_TIP,
      mp_hands.HandLandmark.MIDDLE_FINGER_TIP,
      mp_hands.HandLandmark.RING_FINGER_TIP,
      mp_hands.HandLandmark.PINKY_TIP
    ]
    landmark_coordinates = [(hand_landmarks.landmark[landmark].x, hand_landmarks.landmark[landmark].y, hand_landmarks.landmark[landmark].z) for landmark in target_landmarks]
    wrist_coordinate = [hand_landmarks.landmark[mp_hands.HandLandmark.WRIST].x , hand_landmarks.landmark[mp_hands.HandLandmark.WRIST].y, hand_landmarks.landmark[mp_hands.HandLandmark.WRIST].z ]
    mmcp_coordinate = [hand_landmarks.landmark[mp_hands.HandLandmark.MIDDLE_FINGER_MCP].x , hand_landmarks.landmark[mp_hands.HandLandmark.MIDDLE_FINGER_MCP].y, hand_landmarks.landmark[mp_hands.HandLandmark.MIDDLE_FINGER_MCP].z ]
    cmc_coordinate = [hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_MCP].x , hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_MCP].y, hand_landmarks.landmark[mp_hands.HandLandmark.INDEX_FINGER_MCP].z ]
    scale = sqrt(abs(cmc_coordinate[2]-mmcp_coordinate[2])**2+abs(cmc_coordinate[1]-mmcp_coordinate[1])**2+abs(cmc_coordinate[0]-mmcp_coordinate[0])**2)    
    distance_from_wrist = [sqrt(abs(finger_coordinate[2]-wrist_coordinate[2])**2+abs(finger_coordinate[1]-wrist_coordinate[1])**2+abs(finger_coordinate[0]-wrist_coordinate[0])**2) for finger_coordinate in landmark_coordinates]
    return landmark_coordinates, distance_from_wrist

def get_fingers_up(hand_landmarks):
  landmark_coordinates, distance_from_wrist = get_fingers_up_numbers(hand_landmarks)
  min_ups = [0.4, 0.5, 0.6, 0.4, 0.5]
  return [distance_from_wrist[i]>=min_ups[i] for i in range(len(min_ups))], distance_from_wrist

def get_arrrows(hand_landmarks, finger_ups, image):
    arrows = [False]*4
    if finger_ups[1]:
        return arrows

    image_height, image_width, _ = image.shape
    thumb_coordinate = [hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP].x , hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP].y, hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP].z ]
    
    if  (thumb_coordinate[0]*image_width < (image_width*0.33)):
        arrows[0] = True #left
    if  (thumb_coordinate[0]*image_width >= (image_width*0.7)):
        arrows[3] = True #right
    if (thumb_coordinate[1]*image_height < ((image_height*33))):
        arrows[1] = True # up
    if(thumb_coordinate[1]*image_height >= (image_height*0.7)):
        arrows[2] = True # down
    
    return arrows

def get_hand():
    global message
  
    cap = cv2.VideoCapture(0)
    with mp_hands.Hands(
        model_complexity=0,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5) as hands:
        while cap.isOpened():
            success, image = cap.read()
            if not success:
                print("Ignoring empty camera frame.")
                # If loading a video, use 'break' instead of 'continue'.
                continue

            # To improve performance, optionally mark the image as not writeable to
            # pass by reference.
            image.flags.writeable = False
            image_height, image_width, _ = image.shape
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            results = hands.process(image)

            # Draw the hand annotations on the image.
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
            if results.multi_hand_landmarks:
                for hand_landmarks in results.multi_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image,
                        hand_landmarks,
                        mp_hands.HAND_CONNECTIONS,
                        mp_drawing_styles.get_default_hand_landmarks_style(),
                        mp_drawing_styles.get_default_hand_connections_style())
                    # message = str(hand_landmarks)
                    # print('message:', message)         
                    # print(get_fingers_up(hand_landmarks, image)[0])               
                    fingers_ups = get_fingers_up(hand_landmarks)[0]

                    message = {"fingers_up": get_arrrows(hand_landmarks, fingers_ups, image)}
                    # message =  (image_height, image_width, [hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP].x*image_width , hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP].y*image_height, hand_landmarks.landmark[mp_hands.HandLandmark.THUMB_TIP].z ], get_arrrows(hand_landmarks, fingers_ups, image))
                # Flip the image horizontally for a selfie-view display.

            image = cv2.resize(image, (1920, 1080))
            cv2.imshow('Hand recog', cv2.flip(image, 1))
            if (cv2.waitKey(5) & 0xFF == 27) and running:
                break
        cap.release()




def initiate_client():
    global client_socket
    host = "127.0.0.1"#socket.gethostname()  # as both code is running on same pc
    port = 8055  # socket server port number    
    client_socket = socket.socket()  # instantiate
    print('trying to connect...')
    while running:
        try:
            client_socket.connect((host, port))  # connect to the server
            print('connection success')
            break
        except ConnectionRefusedError:
            print('will retry connecting...')
            time.sleep(2)


def client_program():
    print('client initiating...')
    initiate_client()
    print('client has been initialized')

    # message = input('-> ')

    while  running:
        print('send', message)
        try:
            client_socket.send(json.dumps(message).encode()+b'\0')  # send message
        except ConnectionError:
            initiate_client()
        # data = client_socket.recv(1024).decode()  # receive response

        # print('Received from server: ' + data)  # show in terminal

        # message = input(" -> ")  # again take input

    client_socket.close()  # close the connection
    print('client done')

def print_message():
    while running:
        print(message)

if __name__ == '__main__':
    # client_program()

    Thread(target=client_program).start()
    time.sleep(3)
    try:
        get_hand()
    except KeyboardInterrupt:
        running = False
        exit(0)

