import os

def print_directory_contents(path, indent=''):
    """
    Print the directory structure recursively along with all the files within it.
    """
    for item in os.listdir(path):
        item_path = os.path.join(path, item)
        if os.path.isdir(item_path):
            print(indent + '|-- ' + item + '/')
            print_directory_contents(item_path, indent + '    ')
        else:
            print(indent + '|-- ' + item)

# Replace 'directory_path' with the path of the directory you want to traverse
directory_path = './'

print('Directory structure of', directory_path)
print_directory_contents(directory_path)